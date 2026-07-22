/**
 * TODO: every enum in this file is declared by the backend Swagger as a bare
 * integer with no x-enumNames/prose mapping published anywhere in
 * notification-swagger.json. Per "do not invent missing fields," all are kept
 * as opaque `number` until the backend documents their value→name mapping.
 * See docs/backend/notification/README.md "Known limitations."
 */
export type AudienceType = number; // enum [1,2,3,4]
export type CampaignExecutionType = number; // enum [1,2]
export type CampaignStatus = number; // enum [1,2,3,4,5]
export type ChannelValidationStatus = number; // enum [1,2,3]
export type NotificationChannelStatus = number; // enum [1,2]
export type NotificationChannelType = number; // enum [1,2,3,4,5,6,7]
export type DispatchStatus = number; // enum [1,2,3,4,5]
export type NotificationGroupStatus = number; // enum [1,2]
export type NotificationPriority = number; // enum [1,2,3,4]
export type NotificationRuleStatus = number; // enum [1,2]
export type NotificationStatus = number; // enum [1,2,3]
export type NotificationTemplateStatus = number; // enum [1,2]
