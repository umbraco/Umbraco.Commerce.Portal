import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export type UcpInstallerModalSubmitValue = {
  selected: string;
}

export const UCP_INSTALLER_MODAL_ALIAS = 'Umbraco.Commerce.Portal.InstallerConfigModal';

export const UCP_INSTALLER_MODAL_TOKEN = new UmbModalToken<object, UcpInstallerModalSubmitValue>(
  UCP_INSTALLER_MODAL_ALIAS, {
  modal: {
    type: 'sidebar',
    size: 'small',
  },
});
