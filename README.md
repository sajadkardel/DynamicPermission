# DynamicPermission🚷

Using the tricks in this project, you can design a dynamic access level system that applies to all requests that can be sent by the program.
In short, each user has roles, and each role has claims that are the same urls or accessible requests in that role.

In this way, any user who has the relevant role can access that particular request or url.
The strength of this trick is that you can apply the access level for the smallest details and control all the actions of the program.

In this type of access level application, the full url name, including Area / Controller / Action, is usually displayed, and the program admin is faced with the names of program files and folders for managing access, but in this project, a feature has been added that you using DisplayName Attribute, then You can display the custom name that the admin notices.

In total, all your actions must contain two attributes that indicate Authorized or not Authorized to  that action.
Actions that use DisplayName Attribute are Needing To have Authorize and which use the AllowAnonymous Attrubite are accessible for everyone.
