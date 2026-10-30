import type { UmbEntryPointOnInit } from '@umbraco-cms/backoffice/extension-api';

import { manifests as dashboardManifest } from './backoffice/dashboards/manifest';

import './css/auth/ucportal-auth.css';
import './css/management/ucportal-management.css';
import './css/management/ucportal-management.responsive.css';
import './css/management/ucportal-order-status.css';

const manifests: Array<UmbExtensionManifest> = [
  ...dashboardManifest,
];

export const onInit: UmbEntryPointOnInit = (_host, extensionRegistry) => {
  extensionRegistry.registerMany(manifests);
};
