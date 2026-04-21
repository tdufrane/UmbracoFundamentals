import {
  LitElement,
  html,
  customElement,
  property,
  state,
  ifDefined,
} from "@umbraco-cms/backoffice/external/lit";
import { UmbChangeEvent } from "@umbraco-cms/backoffice/event";
import type { UmbPropertyEditorConfigCollection } from "@umbraco-cms/backoffice/property-editor";

@customElement("my-propertyeditortraining")
export class PropertyEditorTraining extends LitElement {
  @property({ type: String })
  public value = "";

  @state()
  private _renderedValue = "";

  @state()
  private _umbazeIndex = ["H5YR!", "Umbazing!", "Unicorns!"];

  @state()
  private _disabled?: boolean;

  @state()
  private _placeholder?: string;

  @property({ attribute: false })
  public set config(config: UmbPropertyEditorConfigCollection) {
    this._disabled = config.getValueByAlias("disabled");
    this._placeholder = config.getValueByAlias("placeholder");
  }

  render() {
    return html`
      <input
        .value=${this.value || ""}
        placeholder=${ifDefined(this._placeholder)}
        @input="${this.#updateValue}"
        style="width: 400px;"
      />
      <button ?disabled=${this._disabled} @click=${this.#onClick}>
        Umbaze
      </button>
      <hr />
      ${this._renderedValue}
    `;
  }

  #updateValue(e: InputEvent) {
    this._renderedValue = (e.target as HTMLInputElement).value;
    this.value = this._renderedValue;
    this.#dispatchChangeEvent();
  }

  #onClick() {
    const randomIndex = (this._umbazeIndex.length * Math.random()) | 0;
    this.value = (this.value ?? "") + " " + this._umbazeIndex[randomIndex];
    this._renderedValue = this.value;
    this.#dispatchChangeEvent();
  }

  #dispatchChangeEvent() {
    this.dispatchEvent(new UmbChangeEvent());
  }
}

export { PropertyEditorTraining as default };
