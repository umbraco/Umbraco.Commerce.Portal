import type { ManifestDashboard, ManifestModal } from "@umbraco-cms/backoffice/extension-registry";
import { UCP_INSTALLER_MODAL_ALIAS } from "../modals/installer-modal.token";

const dashboardManifests: Array<ManifestDashboard | ManifestModal> = [
  {
    type: "dashboard",
    alias: "Umbraco.Commerce.Portal.InstallerDashboard",
    weight: -100,
    name: "Umbraco Commerce Portal",
    meta: {
    },
    element: () => import("./installer-dashboard.element"),
    elementName: "uc-portal-installer-dashboard",
    conditions: [
      {
        "alias": "Umb.Condition.SectionAlias",
        "match": "Umb.Section.Settings",
      },
    ],
  },
  {
    type: "modal",
    alias: UCP_INSTALLER_MODAL_ALIAS,
    meta: {},
    name: "Umbraco Commerce Portal Installer Modal",
    elementName: "ucp-installer-config-modal",
    element: () => import("../modals/installer-modal.element"),
  },
];

export const manifests = [...dashboardManifests];
