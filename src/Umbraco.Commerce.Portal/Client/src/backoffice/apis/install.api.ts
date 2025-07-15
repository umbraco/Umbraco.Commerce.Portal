export const OpenApiConfig: {
  credentials: RequestCredentials,
  token: () => Promise<string | undefined>,
} = {
  credentials: 'same-origin',
  token: async () => '',
};
