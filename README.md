
# Custom Security for Power BI Report Server and SQL Reporting Services, using the OpenID Connect protocol.
This project contains a sample and the steps that allow you to deploy a custom security extension to Power BI Report Server.

# Prerequisites

- [.NET Framework 4.8 Developer Pack](https://dotnet.microsoft.com/download/dotnet-framework/net48)
- [Microsoft Visual Studio Community 2022](https://docs.microsoft.com/en-us/visualstudio/releases/2022/release-notes)
- [Microsoft Power BI Report Server - May 2024](https://www.microsoft.com/en-us/download/details.aspx?id=105944&culture=en-us&country=us)
- [SQL Server 2019](https://www.microsoft.com/en-us/evalcenter/evaluate-sql-server-2019?filetype=EXE)

**How to Build .NET Project with CLI**

\# Open `Developer Command Prompt for VS 2019` and goto the project directory, then type the follow commands:

```dos
> msbuild -t:Clean <1>
> msbuild -t:Build <2>
> msbuild <3>
> msbuild -p:Configuration=Release <4>
```
- <1> Clean build outputs of a .NET project.
- <2> Build a .NET project.
- <3> Build a .NET project with default `build` target.
- <4> Build a .NET project with `Release` configuration.

\# Open any Command Prompt or Bash Console and goto the project direcotry, then type the follow commands:

```sh
$ dotnet clean <1>
$ dotnet build <2>
$ dotnet build -c Release <3>
```

- <1> Clean build outputs of a .NET project.
- <2> Build a .NET project.
- <3> Build a .NET project with `Release` configuration.


# Reporting Services Custom Security

You must first compile and install the extension. The procedure assumes that you have installed Reporting Services to the default location: `C:\Program Files\Microsoft Power BI Report Server\PBIRS`. This location will be referred to throughout the remainder of this topic as `<install>`.

## Step 1. Build the Extension Project

```console
$ dotnet build
  Determining projects to restore...
  Nothing to do. None of the projects specified contain packages to restore.
C:\Program Files\dotnet\sdk\8.0.400\Microsoft.CSharp.CurrentVersion.targets(129,9): warning MSB3884: Could not find rule set file "AllRules.ruleset". [C
:\Users\vuhoang\Desktop\source\powerbi-reporting-server-extensions\CustomSecurity.csproj]
  CustomSecurity -> C:\Users\vuhoang\Desktop\source\powerbi-reporting-server-extensions\bin\Debug\PowerBI.ReportingServer.Extensions.dll

Build succeeded.

C:\Program Files\dotnet\sdk\8.0.400\Microsoft.CSharp.CurrentVersion.targets(129,9): warning MSB3884: Could not find rule set file "AllRules.ruleset". [C
:\Users\vuhoang\Desktop\source\powerbi-reporting-server-extensions\CustomSecurity.csproj]
    1 Warning(s)
    0 Error(s)

Time Elapsed 00:00:01.18
```

**To compile the Project using Visual Studio**

- Open `CustomSecurity.sln` in Microsoft Visual Studio.
- In Solution Explorer, select the CustomSecurity project.
- Look at the CustomSecurity project's Dependencies-&gt;Assemblies.

  - If you do not see `Microsoft.ReportingServices.Interfaces.dll`, then complete the following steps:
    - On the Assemblies menu, right click `Add Assembly Reference...`. The Add References dialog box opens.
    - Click Browse, and find Microsoft.ReportingServices.Interfaces on your local drive. By default, the assembly is in the `<install>\ReportServer\bin` directory. Click OK. The selected reference is added to your project.
- On the Build menu, click Build Solution.

**Debugging**

To debug the extension, you might want to attach the debugger to both `ReportingServicesService.exe`, `RSPortal.exe` and `RSPowerBI.exe`. And add breakpoints to the methods implementing the interface `IAuthenticationExtension2` and `IAuthorizationExtension`.

**Troubleshooting**

If the report server cann't work expectedly, you may need to analyze the reprot server log files at `<install>/LogFiles`, especially the following highlight files :

```connsole
$ ls -1 LogFiles/
ASManagedRoot_2024_10_11_09_56_18.log
FlightRecorderCurrent.trc
msmdsrv.log
ReportingServicesService_2024_10_11_09_56_17.log
ReportingServicesWMI_2024_10_11_09_54_26.log
RSHostingService_2024_10_11_09_55_46.log
RSManagement_2024_10_11_09_55_47.log
RSOffice_2024_10_11_09_56_17.log
RSPortal_2024_10_11_09_56_17.log
RSPowerBI_2024_10_11_09_56_17.log
```

## Step 2: Deployment and Configuration

```console
# The Build Outputs of the Security Extenstion Project
$ ls ./powerbi-reporting-server-extensions/bin/Debug/
PowerBI.ReportingServer.Extensions.dll*  Microsoft.ReportingServices.Interfaces.dll*
PowerBI.ReportingServer.Extensions.pdb   Sso.aspx
```

### To deploy the security extension

- Copy the `Sso.aspx` page to the `<install>\ReportServer` directory.
- Copy all the `\*.dll` and `*.pdb` to the follow directories:
  - `<install>\ReportServer\bin`
  - `<install>\Portal`
  - `<install>\PowerBI`

### Modify files in the `<install>\ReportServer` direcotry

- To modify the `rsreportserver.config` file.
- Open the `rsreportserver.config` file with Visual Studio or a simple text editor such as Notepad. `rsreportserver.config` is located in the `<install>\ReportServer` directory.

- Locate the `<AuthenticationTypes>` element and modify the settings as follows:

  ```xml
  <Authentication>
    <AuthenticationTypes>
      <!--<RSWindowsNTLM/>-->
      <Custom/>
    </AuthenticationTypes>
    <RSWindowsExtendedProtectionLevel>Off</RSWindowsExtendedProtectionLevel>
    <RSWindowsExtendedProtectionScenario>Proxy</RSWindowsExtendedProtectionScenario>
    <EnableAuthPersistence>true</EnableAuthPersistence>
  </Authentication>
  ```
  
  - <1> Note that you cannot use Custom with other authentication types.

* Locate the `<Security>` and `<Authentication>` elements, within the `<Extensions>` element, and modify the settings as follows:

  ```xml
  <Security>
    <Extension Name="Forms" Type="PowerBI.ReportingServer.Extensions.OIDC.Authorization, PowerBI.ReportingServer.Extensions">
      <Configuration>
        <AdminConfiguration>
          <UserName>hoangvubrvt@gmail.com</UserName>
        </AdminConfiguration>
      </Configuration>
    </Extension>
    <!--<Extension Name="Windows" Type="Microsoft.ReportingServices.Authorization.WindowsAuthorization, Microsoft.ReportingServices.Authorization"/>-->
  </Security>
  ```
  
  - <1> Note that you should specify one or many administrators here.
  
  ```xml
  <Authentication>
    <Extension Name="Forms" Type="PowerBI.ReportingServer.Extensions.OIDC.Authentication, PowerBI.ReportingServer.Extensions"/>
    <!--<Extension Name="Windows" Type="Microsoft.ReportingServices.Authentication.WindowsAuthentication, Microsoft.ReportingServices.Authorization"/>-->
  </Authentication>
  ```

### To modify the `web.config` file for Report Server

- Open the `web.config` file in a text editor. By default, the file is in the `<install>\ReportServer` directory.
- Locate the `<identity>` element and set the `Impersonate` attribute to `false`.

  ```xml
  <identity impersonate="false" />
  <!--<identity impersonate="true" />-->
  ```

- Locate the `<authentication>` element and change the `Mode` attribute to `Forms`. Also, add the following `<forms>` element as a child of the `<authentication>` element and set the `loginUrl`, `name`, `timeout`, `path`, `requireSSL`, and `cookieSameSite` attributes as follows:

  ```xml
  <!--<authentication mode="Windows" />-->
  <authentication mode="Forms">
    <forms loginUrl="Sso.aspx" name="X-RS-TOKEN" timeout="60" path="/" requireSSL="true" cookieSameSite="None">
    </forms>
  </authentication>
  ```
  
  For local development, if you cann't debug with HTTPS, you should delete both the `requireSSL` and `cookieSameSite` attributes.
  
  ```xml
  <!--<authentication mode="Windows" />-->
  <authentication mode="Forms">
    <forms loginUrl="Sso.aspx" name="X-RS-TOKEN" timeout="60"  path="/">
    </forms>
  </authentication>
  ```

- Add the following `<authorization>` element directly after the `<authentication>` element.

  ```xml
  <authorization>
    <deny users="?" />
  </authorization>
  ```
  
  This will deny unauthenticated users the right to access the report server. The previously established `loginUrl` attribute of the `<authentication>` element will redirect unauthenticated requests to the `Sso.aspx` page.

- Configuration `<appSettings>` inner the element `<configuration>` as below. (https://corptb.sharepoint.com/sites/identityaccessmanagement/sitepages/qg071.aspx?web=1#qa-environment)

  ```xml
  <appSettings>
    <add key="oauth.baseurl" value="https://login.microsoftonline.com" />
    <add key="oauth.tenant.id" value="***" />
    <add key="oauth.client.id" value="***" />
    <add key="oauth.client.secret" value="***" />
  </appSettings>
  ```
  
- Locate the `<trust>` element and update it as follows:

```xml
<!--<securityPolicy>
  <trustLevel name="RosettaSrv" policyFile="rssrvpolicy.config" />
</securityPolicy>
<trust level="RosettaSrv" originUrl="" egacyCasModel="true" />-->
<trust level="Full" />
```

## Step 3: Generate Machine Keys

Using *Forms* authentication requires that all report server processes can access the authentication cookie. This involves configuring a machine key and decryption algorithm -- a familiar step for those who had previously setup SSRS to work in scale-out environments.

Generate and add `<MachineKey>` under `<Configuration>` in your `rsreportserver.config` file.

```xml
<MachineKey ValidationKey="[YOUR KEY]" DecryptionKey="[YOUR KEY]" Validation="AES" Decryption="AES" />
```

The follow code snippet is a sample:

```xml
<Configuration>
  <MachineKey 
    ValidationKey="C9A00A9C93B7AC6B8B3C27054DEDA40FDE08D20C481E808042F32784B3A7F5EF" 
    DecryptionKey="8F3D5F7B29A0EB685B61299502490226DA98BCB73B024F78651C24517A5ACCB9"
    Validation="AES" 
    Decryption="AES"/>
    . . .
```

**Check the casing of the attributes, it should be Pascal Casing as the example above.**

There is not need for a `<system.web>` entry.

You should use a validation key specific for you deployment, there are several tools to generate the keys such as Internet Information Services Manager (IIS), or the online [machine-key-generator](https://codewithshadman.com/machine-key-generator/).

## Step 4: Configure Passthrough cookies

The new portal and the reportserver communicate using internal soap APIs for some of its operations. When additional cookies are required to be passed from the portal to the server the `PassThroughCookies` properties is still available. More Details: https://msdn.microsoft.com/en-us/library/ms345241.aspx. In the `rsreportserver.config` file add following under `<UI>`.

```xml
<UI>
  <ReportServerUrl></ReportServerUrl>
  <PageCountMode>Estimate</PageCountMode>
  <CustomAuthenticationUI>
    <PassThroughCookies>
      <PassThroughCookie>X-RS-TOKEN</PassThroughCookie>
    </PassThroughCookies>
  </CustomAuthenticationUI>
</UI>
```

# Security Extensions Overview

Reporting Services provides an extensible architecture that allows you to plug in custom or forms-based authentication modules. You might consider implementing a custom authentication extension if deployment requirements do not include Windows integrated security or Basic authentication. The most common scenario for using custom authentication is to support Internet or extranet access to a Web application. Replacing the default Windows Authentication extension with a custom authentication extension gives you more control over how external users are granted access to the report server.

In practice, deploying a custom authentication extension requires multiple steps that include copying assemblies and application files, modifying configuration files, and testing. 

NOTE: Creating a custom authentication extension requires custom code and expertise in ASP.NET security. If you do not want to create a custom authentication extension, you can use Microsoft Active Directory groups and accounts, but you should greatly reduce the scope of a report server deployment. For more information about custom authentication, see https://docs.microsoft.com/en-us/sql/reporting-services/extensions/security-extension/implementing-a-security-extension?view=sql-server-ver15[Implementing a Security Extension].

We recommend that you use Windows Authentication if at all possible. However, custom authentication and authorization for Reporting Services may be appropriate in the following two cases:

- You have an Internet or extranet application that cannot use Windows accounts.

- You have custom-defined users and roles and need to provide a matching authorization scheme in Reporting Services.

![Security Extensions Overview](https://docs.microsoft.com/en-us/sql/reporting-services/extensions/security-extension/media/rosettasecurityextensionflow.gif?view=sql-server-ver15.gif)

As shown in the above figure, authentication and authorization occur as follows:

- <1> A user tries to access the web portal by using a URL and is redirected to a form that collects user credentials for the client application.
 
- <2> The user submits credentials to the form.
 
- <3> The user credentials are submitted to the Reporting Services Web service through the LogonUser method.
 
- <4> The Web service calls the customer-supplied security extension and verifies that the user name and password exist in the custom security authority.
 
- <5> After authentication, the Web service creates an authentication ticket (known as a "cookie"), manages the ticket, and verifies the user's role for the Home page of the web portal.
 
- <6> The Web service returns the cookie to the browser and displays the appropriate user interface in the web portal.
 
- <7> After the user is authenticated, the browser makes requests to the web portal while transmitting the cookie in the HTTP header. These requests are in response to user actions within the web portal.
 
- <8> The cookie is transmitted in the HTTP header to the Web service along with the requested user operation.
 
- <9> The cookie is validated, and if it is valid, the report server returns the security descriptor and other information relating to the requested operation from the report server database.
 
- <10> If the cookie is valid, the report server makes a call to the security extension to check if the user is authorized to perform the specific operation.
 
- <11> If the user is authorized, the report server performs the requested operation and returns control to the caller.
 
- <12> After the user is authenticated, URL access to the report server uses the same cookie. The cookie is transmitted in the HTTP header.
 
- <13> The user continues to request operations on the report server until the session has ended.

## Authentication in Reporting Services

[iauthenticationextension2-url]: https://docs.microsoft.com/en-us/dotnet/api/microsoft.reportingservices.interfaces.iauthenticationextension2
[iextension-url]: https://docs.microsoft.com/en-us/dotnet/api/microsoft.reportingservices.interfaces.iextension
[microsoftreportingservicesinterfaces-url]: https://docs.microsoft.com/en-us/dotnet/api/microsoft.reportingservices.interfaces
[iauthorizationextension-url]: https://docs.microsoft.com/en-us/dotnet/api/microsoft.reportingservices.interfaces.iauthorizationextension

Authentication is the process of establishing a user's right to an identity. There are many techniques that you can use to authenticate a user. The most common way is to use passwords. When you implement Forms Authentication, for example, you want an implementation that queries users for credentials (usually by some interface that requests a login name and password) and then validates users against a data store, such as a database table or configuration file. If the credentials can't be validated, the authentication process fails and the user will assume an anonymous identity.

In Reporting Services, the Windows operating system handles the authentication of users either through integrated security or through the explicit reception and validation of user credentials. Custom authentication can be developed in Reporting Services to support additional authentication schemes. This is made possible through the security extension interface [IAuthenticationExtension2][iauthenticationextension2-url]. All extensions inherit from the [IExtension][iextension-url] base interface for any extension deployed and used by the report server. [IExtension][iextension-url], as well as [IAuthenticationExtension2][iauthenticationextension2-url], are members of the [Microsoft.ReportingServices.Interfaces][microsoftreportingservicesinterfaces-url] namespace.

![Authentication Flow](https://docs.microsoft.com/en-us/sql/reporting-services/extensions/security-extension/media/rosettasecurityextensionauthenticationflow.gif?view=sql-server-ver15)

As shown in the above figure, the authentication process is as follows:

[logonuser-url]: https://docs.microsoft.com/en-us/dotnet/api/microsoft.reportingservices.interfaces.iauthenticationextension2.logonuser?view=sqlserver-2016

- <1> A client application calls the Web service [LogonUser][logonuser-url] method to authenticate a user.
- <2> The Web service makes a call to the [LogonUser][logonuser-url] method of your security extension, specifically, the class that implements https://docs.microsoft.com/en-us/dotnet/api/microsoft.reportingservices.interfaces.iauthenticationextension2?view=sqlserver-2016[IAuthenticationExtension2].
- <3> Your implementation of [LogonUser][logonuser-url] validates the user name and password in the user store or security authority.
- <4> Upon successful authentication, the Web service creates a cookie and manages it for the session.
- <5> The Web service returns the authentication ticket to the calling application on the HTTP header.

## Authorization in Reporting Services

Authorization is the process of determining whether an identity should be granted the requested type of access to a given resource in the report server database. Reporting Services uses a role-based authorization architecture that grants a user access to a given resource based on the user's role assignment for the application. Security extensions for Reporting Services contain an implementation of an authorization component that is used to grant access to users once they are authenticated on the report server. Authorization is invoked when a user attempts to perform an operation on the system or a report server item through the SOAP API and via URL access. This is made possible through the security extension interface [IAuthorizationExtension][iauthorizationextension-url]. As stated previously, all extensions inherit from [IExtension][iextension-url] the base interface for any extension that you deploy. [IExtension][iextension-url] and [IAuthorizationExtension][iauthorizationextension-url] are members of the [Microsoft.ReportingServices.Interfaces][microsoftreportingservicesinterfaces-url] namespace.

![Authorization Flow](https://docs.microsoft.com/en-us/sql/reporting-services/extensions/security-extension/media/rosettasecurityextensionauthorizationflow.gif?view=sql-server-ver15)

As shown in the Figure 3, authorization follows this sequence:

[checkaccess-url]: https://docs.microsoft.com/en-us/dotnet/api/microsoft.reportingservices.interfaces.iauthorizationextension.checkaccess
[getuserinfo]: https://docs.microsoft.com/en-us/dotnet/api/microsoft.reportingservices.interfaces.iauthenticationextension.getuserinfo

- <1> Once authenticated, client applications make requests to the report server through the Reporting Services Web service methods. An authentication ticket is passed to the report server in the form of a cookie in the HTTP header of each Web request.
- <2> The cookie is validated prior to any access check.
- <3> Once the cookie is validated, the report server calls [GetUserInfo][getuserinfo] and the user is given an identity.
- <4> The user attempts an operation through the Reporting Services Web service.
- <5> The report server calls the [CheckAccess][checkaccess-url] method.
- <6> The security descriptor is retrieved and passed to a custom security extension implementation of [CheckAccess][checkaccess-url]. At this point, the user, group, or computer is compared to the security descriptor of the item being accessed and is authorized to perform the requested operation.
- <7> If the user is authorized, the Web service performs the operation and returns a response to the calling application.


# References
- https://github.com/ousiax/PowerBI.ReportingServices.Extensions
- https://openid.net/developers/how-connect-works
- https://docs.microsoft.com/en-us/power-bi/report-server/get-started, What is Power BI Report Server?
- https://docs.microsoft.com/en-us/power-bi/report-server/install-report-server, Install Power BI Report Server
- https://docs.microsoft.com/en-us/power-bi/report-server/install-powerbi-desktop, Install Power BI Desktop for Power BI Report Server
- https://www.microsoft.com/en-us/sql-server/sql-server-downloads, SQL Server Downloads | Microsoft
- https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms?view=sql-server-ver15, Download SQL Server Management Studio (SSMS)
- https://docs.microsoft.com/en-us/sql/reporting-services/report-server/reporting-services-configuration-files?view=sql-server-ver15, Reporting Services Configuration Files
- https://docs.microsoft.com/en-us/sql/reporting-services/report-server/reporting-services-log-files-and-sources?view=sql-server-ver15, Reporting Services Log Files and Sources
- https://docs.microsoft.com/en-us/sql/reporting-services/security/authentication-with-the-report-server?view=sql-server-ver15, Authentication with the Report Server
- https://docs.microsoft.com/en-us/sql/reporting-services/extensions-ssrs?view=sql-server-ver15, Extensions for SQL Server Reporting Services (SSRS)
- https://docs.microsoft.com/en-us/sql/reporting-services/extensions/security-extension/security-extensions-overview?view=sql-server-ver15, Security Extensions Overview - Reporting Services (SSRS)
- https://docs.microsoft.com/en-us/sql/reporting-services/extensions/security-extension/authentication-in-reporting-services?view=sql-server-ver15, Authentication in Reporting Services
- https://docs.microsoft.com/en-us/sql/reporting-services/extensions/security-extension/authorization-in-reporting-services?view=sql-server-ver15, Authorization in Reporting Services
- https://docs.microsoft.com/en-us/sql/reporting-services/extensions/secure-development/using-reporting-services-security-policy-files?view=sql-server-ver15#placement-of-codegroup-elements-for-extensions, Placement of CodeGroup Elements for Extensions
- https://docs.microsoft.com/en-us/previous-versions/dotnet/netframework-1.1/b5ysx397(v=vs.71), ASP.NET Settings Schema
- https://docs.microsoft.com/en-us/previous-versions/, Previous versions of Microsoft products, services and technologies
- https://www.entityframeworktutorial.net/code-first/automated-migration-in-code-first.aspx, Automated Migration in Entity Framework 6
- https://codewithshadman.com/machine-key-generator/, Machine Key Generator

