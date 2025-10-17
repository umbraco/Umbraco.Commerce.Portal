import { LitElement, html, css, customElement } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import { umbOpenModal } from "@umbraco-cms/backoffice/modal";
import { UCP_INSTALLER_MODAL_TOKEN } from "../modals/installer-modal.token";

const ELEMENT_NAME = "uc-portal-installer-dashboard";
@customElement(ELEMENT_NAME)
export class UcPortalInstallerDashboard extends UmbElementMixin(LitElement) {

  #onOpenRootPickerClick() {
    umbOpenModal(this, UCP_INSTALLER_MODAL_TOKEN, {
      data: {},
    });
  }

  render() {
    return html`
      <umb-body-layout header-transparent>
        <uui-box>
          <div class="ucp-installer-wrapper">
              <!-- Header -->
              <div>
                  <div style="display: inline-flex; align-items: center; justify-content: center; background-color: #141432; width: 120px; height: 120px; border-radius: 100%;">
                    <uui-icon name="icon-client" style="color: white; font-size: 80px;"></uui-icon>
                  </div>
              </div>
              <div class="installer-intro" style="margin-bottom: 5px;">
                  <h3>Customer Portal</h3>
              </div>
              <p>
                  Umbraco Commerce Portal provides a customer portal for Umbraco Commerce
              </p>

              <!-- Installer -->
              <h4>Getting Started</h4>
              <p style="margin-bottom: 10px;">
                To get started with Umbraco Commerce Portal we first need to install all the related content.<br />
                By clicking the <strong>Install</strong> button below Umbraco Commerce Portal will install all the Data Types,<br />
                Doc Types and Content nodes needed.
              </p>
              <p>
                If you have installed Umbraco Commerce Portal before, the installer will also perform the relevant upgrades.<br />
                <br /><strong>NB: Nothing</strong> will be removed as part of an upgrade.
              </p>
              <p style="margin-top: 30px;">
                  <uui-button
                    look="primary"
                    label="Install"
                    type="button"
                    @click=${this.#onOpenRootPickerClick}></uui-button>
              </p>
          </div>
        </uui-box>
      </umb-body-layout>
    `;
  }

  static styles = css`
    .ucp-installer-wrapper {
      margin: 20px auto 0;
      text-align: center;
      font-size: 15px;
    }

    h3 {
      font-size: 36px;
      font-weight: 700;
      letter-spacing: -1px;
      line-height: 80px;
      margin: 0 0 0 20px;
    }

    h4 {
      margin-top: 40px;
      font-weight: bold;
      font-size: 18.75px;
    }
  `;
}

export default UcPortalInstallerDashboard;

declare global {
  interface HTMLElementTagNameMap {
    [ELEMENT_NAME]: UcPortalInstallerDashboard;
  }
}
