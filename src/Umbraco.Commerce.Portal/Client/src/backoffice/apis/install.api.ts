export const OpenApiConfig: {
  credentials: RequestCredentials,
  token: () => Promise<string | undefined>,
} = {
  credentials: 'same-origin',
  token: async () => '',
};

export type InstallUcpApiResponse = {
  success: boolean,
  message?: string
}

export const installUmbracoCommercePortalAsync: (siteRootNodeId: string) => Promise<InstallUcpApiResponse> = async (siteRootNodeId: string) => {
  const response = fetch("/umbraco/management/api/v1/umbraco-commerce-portal/install?siteRootNodeId=" + siteRootNodeId, {
    credentials: OpenApiConfig.credentials,
    headers: {
      "Authorization": "Bearer " + await OpenApiConfig.token(),
    },
  }).then(
    (response: Response) => {
      return response.json() as Promise<InstallUcpApiResponse>;
    },
    (reason: unknown) => ({
      success: false,
      message: JSON.stringify(reason),
    })) as Promise<InstallUcpApiResponse>;

  return response;
};

