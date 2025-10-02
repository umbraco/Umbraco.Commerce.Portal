import type { UmbEntryPointOnInit } from "@umbraco-cms/backoffice/extension-api";

import { UMB_AUTH_CONTEXT } from "@umbraco-cms/backoffice/auth";
import { manifests as dashboardManifest } from "./backoffice/dashboards/manifest";
import { OpenApiConfig } from "./backoffice/apis/install.api";

import "./css/auth/ucportal-auth.css";
import "./css/management/ucportal-management.css";
import "./css/management/ucportal-management.responsive.css";
import "./css/management/ucportal-order-status.css";

const manifests: Array<UmbExtensionManifest> = [
  ...dashboardManifest,
];

export const onInit: UmbEntryPointOnInit = (_host, extensionRegistry) => {
  extensionRegistry.registerMany(manifests);
  _host.consumeContext(UMB_AUTH_CONTEXT, async (instance) => {
    if (!instance) return;
    const umbOpenApi = instance.getOpenApiConfiguration();
    OpenApiConfig.token = umbOpenApi.token;
  });
};
