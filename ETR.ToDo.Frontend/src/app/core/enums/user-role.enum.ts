/**
 * Mirrors ETR.ToDo.Core.Shared.Enums.UserRole.
 * Values match the string produced by C# enum.ToString() stored in the JWT role claim.
 */
export enum UserRole {
  Basic = 'Basic',
  Premium = 'Premium',
  Admin = 'Admin',
  Dev = 'Dev',
}
