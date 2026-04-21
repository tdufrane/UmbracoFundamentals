import { LitElement, html, customElement } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import { UMB_MODAL_MANAGER_CONTEXT, UmbModalManagerContext } from "@umbraco-cms/backoffice/modal";
import { UMB_ICON_PICKER_MODAL } from '@umbraco-cms/backoffice/icon';

@customElement("my-advancedpropertyeditortraining")
export class MyAdvancedPropertyEditorTraining extends UmbElementMixin(LitElement)  
{
  private _modalContext?: UmbModalManagerContext;

  constructor() {
      super();
      this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (_instance) => {
          this._modalContext = _instance;
    });
  }
  render() {
        return html`
                    <div class="picker">
                        <div>
                            <uui-button look="primary"
                                        color="default"
                                        label="Pick an icon"
                                        @click=${this._OpenIconPicker}></uui-button>
                        </div>
                    </div>
        `;
    }

    async _OpenIconPicker() {
        const modal = UMB_ICON_PICKER_MODAL;
        this._modalContext?.open(this, modal, {value: { color: undefined, icon: undefined }});
    }
}
export{
  MyAdvancedPropertyEditorTraining as default
};