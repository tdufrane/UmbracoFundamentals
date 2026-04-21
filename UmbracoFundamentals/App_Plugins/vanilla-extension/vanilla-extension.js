import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

const template = document.createElement("template");
template.innerHTML = `
  <style>
    :host {
      padding: 20px;
      display: block;
      box-sizing: border-box;
    }
  </style>

  <uui-box>
    <h1>Hello Umbraco</h1>
    <p>This is my vanilla JS element</p>
  </uui-box>
`;

export default class VanillaElement extends UmbElementMixin(HTMLElement) {
  constructor() {
    super();
    this.attachShadow({ mode: "open" });
    this.shadowRoot.appendChild(template.content.cloneNode(true));
  }
}

customElements.define("vanilla-extension", VanillaElement);
