# Blog.net
Task 1: Secured Login

--> User should be able to login with correct username and password.

--> On every successful login user should be able to receive valid token which would be used to authorized before other API request.

Task 2: File Upload

--> CSV data should be exported in database (CSV should have approx.. 50 rows dummy data)

--> Fetch the data from database and bind in Grid with Pagination in another tab.

Task 3: Blog Feedback

--> Blog should be display in to the application page and user should comment on the specific blog.

--> Display blogs with posted comments (history of comments). User can also reply on the any comment. Like and unlike icon button should work with counts.




Run this on package manager console to avoid c#compiler exe file not found error

PM> Uninstall-package Microsoft.CodeDom.Providers.DotNetCompilerPlatform
PM> Uninstall-package Microsoft.Net.Compilers


Remarks

comment section is crude as the page reloads every time a change is made
error handling not ok
[authorize] attribute not used
