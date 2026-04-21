import {
  LitElement,
  html,
  customElement,
  state,
  css,
  repeat,
  ifDefined,
} from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import {
  UmbCurrentUserContext,
  UMB_CURRENT_USER_CONTEXT,
  type UmbCurrentUserModel,
} from "@umbraco-cms/backoffice/current-user";
import {
  type UmbUserDetailModel,
  UmbUserCollectionRepository,
} from "@umbraco-cms/backoffice/user";

import { UMB_EDIT_DOCUMENT_WORKSPACE_PATH_PATTERN } from "@umbraco-cms/backoffice/document";
import { UmbDocumentDetailRepository } from "@umbraco-cms/backoffice/document";
import { UMB_AUTH_CONTEXT } from "@umbraco-cms/backoffice/auth";

@customElement("dashboard-training")
export class DashboardTraining extends UmbElementMixin(LitElement) {
  private _currentUserContext?: UmbCurrentUserContext;

  @state()
  private _currentUser?: UmbCurrentUserModel;

  @state()
  private _userData: Array<UmbUserDetailModel> = [];
  userRepository = new UmbUserCollectionRepository(this);

  @state()
  userAuditData: Array<UserAuditData> = [];

  @state()
  isReady = false;

  documentRepository = new UmbDocumentDetailRepository(this);

  constructor() {
    super();
    this.consumeContext(UMB_CURRENT_USER_CONTEXT, (instance) => {
      this._currentUserContext = instance;
      this.observe(this._currentUserContext?.currentUser, (currentUser) => {
        this._currentUser = currentUser;
      });
    });

    this._getPagedUserData();
  }

  render() {
    if (!this.isReady) {
      return html` <uui-loader-bar style="color: blue"></uui-loader-bar> `;
    }
    if (this.isReady) {
      return html`
        <uui-box headline="Welcome, ${this._currentUser?.name ?? "Unknown"}!">
          <uui-table id="users-wrapper">
            <uui-table-row>
              <uui-table-head-cell></uui-table-head-cell>
              <uui-table-head-cell>Name</uui-table-head-cell>
              <uui-table-head-cell>Email</uui-table-head-cell>
              <uui-table-head-cell>Status</uui-table-head-cell>
              <uui-table-head-cell>Last Login</uui-table-head-cell>
              <uui-table-head-cell>Failed Login Attempts</uui-table-head-cell>
              <uui-table-head-cell
                >Last Edited Content Item</uui-table-head-cell
              >
              <uui-table-head-cell>Edit Item</uui-table-head-cell>
              <uui-table-head-cell>Edit Date</uui-table-head-cell>
            </uui-table-row>
            ${repeat(
              this._userData,
              (user) => user.unique,
              (user) => this._renderUser(user),
            )}
          </uui-table>
        </uui-box>
      `;
    }
  }

  private _renderUser(user: UmbUserDetailModel) {
    if (!user) return;

    const currentUser = this.userAuditData.find(
      (cUser) => cUser.userId == user.unique,
    );

    let shortLoginDate = "";
    if (user.lastLoginDate != null) {
      shortLoginDate = user.lastLoginDate.slice(0, 19).replace("T", " ");
    }
    if (user.lastLoginDate == null) {
      shortLoginDate = "000-00-00 00:00:00";
    }
    let tagColor = "default";
    if (currentUser?.lastEditedDate == "No edits") {
      tagColor = "danger";
    }

    return html`<uui-table-row class="user">
      <uui-table-cell
        ><uui-icon
          style="font-size: 30px; margin-bottom: 9px;"
          name="dashboard-icon-user"
        ></uui-icon
        ><small></small
      ></uui-table-cell>
      <uui-table-cell><b>${user.name}</b></uui-table-cell>
      <uui-table-cell>${user.email}</uui-table-cell>
      <uui-table-cell>${user.state}</uui-table-cell>
      <uui-table-cell>${shortLoginDate}</uui-table-cell>
      <uui-table-cell>${user.failedLoginAttempts}</uui-table-cell>

      <uui-table-cell>${currentUser?.contentItemName}</uui-table-cell>
      <uui-table-cell
        ><a href="${ifDefined(currentUser?.contentItemEditUrl)}">
          <uui-icon
            style="font-size: 20px; margin-bottom: 2px;"
            name="dashboard-icon-edit"
          ></uui-icon
          ><small></small></a
      ></uui-table-cell>
      <uui-table-cell
        ><uui-tag type="${tagColor}" style="font-size: 12px;"
          >${currentUser?.lastEditedDate}</uui-tag
        ></uui-table-cell
      >
    </uui-table-row>`;
  }

  static styles = [
    css`
      :host {
        display: block;
        padding: 24px;
      }

      #users-wrapper {
        border: 1px solid lightgray;
      }
      .user {
        padding: 5px 10px;
      }
      .user:not(:first-child) {
        border-top: 1px solid lightgray;
      }
    `,
  ];

  private async _getPagedUserData() {
    const { data } = await this.userRepository.requestCollection();
    this._userData = data?.items ?? [];
    //sort they array based on the last login date
    this._userData
      .sort(function (a, b) {
        if (a.lastLoginDate != null && b.lastLoginDate != null) {
          return a.lastLoginDate.localeCompare(b.lastLoginDate);
        } else {
          return null as any;
        }
      })
      .reverse();
    //sort the array based on the state so 'Active' will be at the top
    this._userData.sort(function (a, b) {
      if (a.state != null && b.state != null) {
        return a.state.localeCompare(b.state);
      } else {
        return null as any;
      }
    });

    this._getRecentlyEditedContentItems();
  }

  private async _getRecentlyEditedContentItems() {
    const AuthContext: any = await this.getContext(UMB_AUTH_CONTEXT);
    const TOKEN = await AuthContext.getLatestToken();

    const response = await fetch("/api/v1/recently-edited-items", {
      method: "GET",
      headers: { Authorization: `Bearer ${TOKEN}` },
    });

    const data = await response.json();
    this.userAuditData = data;

    await this._addToUserAuditData();
    this.isReady = true;
  }

  private async _addToUserAuditData() {
    for (let i = 0; i < this._userData.length; i++) {
      const currentUser = this.userAuditData.find(
        (cUser) => cUser.userId === this._userData[i].unique,
      );

      if (currentUser) {
        // Check for "No Edits" or "No Edit Date" case
        if (currentUser.lastEditedContentId === "No Edits") {
          currentUser.contentItemName = "NA";
        } else {
          // Only make the request if there's a valid content ID
          const { data } = await this.documentRepository.requestByUnique(
            currentUser.lastEditedContentId,
          );
          if (data && data.variants?.[0]) {
            currentUser.contentItemName = data.variants[0].name;
            const editPath =
              UMB_EDIT_DOCUMENT_WORKSPACE_PATH_PATTERN.generateAbsolute({
                unique: currentUser.lastEditedContentId,
              });
            currentUser.contentItemEditUrl = editPath;
          } else {
            currentUser.contentItemName = "NA";
            currentUser.contentItemEditUrl = "/";
          }
        }
      }
    }
  }
}

type UserAuditData = {
  userId: string;
  lastEditedContentId: string;
  lastEditedDate: string;
  contentItemName: string;
  contentItemEditUrl: string;
};

export { DashboardTraining as default };
