#region
// Copyright (c) 2016 Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License (MIT)
/*============================================================================
  File:      AuthenticationExtension.cs

  Summary:  Demonstrates an implementation of an authentication 
            extension.
--------------------------------------------------------------------
  This file is part of Microsoft SQL Server Code Samples.
    
 This source code is intended only as a supplement to Microsoft
 Development Tools and/or on-line documentation. See these other
 materials for detailed information regarding Microsoft code 
 samples.

 THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF 
 ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO 
 THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
 PARTICULAR PURPOSE.
===========================================================================*/
#endregion

using System;
using System.Security.Principal;
using System.Web;
using Microsoft.ReportingServices.Interfaces;

namespace PowerBI.ReportingServer.Extensions.OIDC
{

    public sealed class Authentication : IAuthenticationExtension2, IExtension
    {


        /// <summary>
        /// You must implement SetConfiguration as required by IExtension
        /// </summary>
        /// <param name="configuration">Configuration data as an XML
        /// string that is stored along with the Extension element in
        /// the configuration file.</param>
        public void SetConfiguration(String configuration)
        {

        }

        /// <summary>
        /// You must implement LocalizedName as required by IExtension
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public string LocalizedName
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Indicates whether a supplied username and password are valid.
        /// </summary>
        /// <param name="userName">The supplied username</param>
        /// <param name="password">The supplied password</param>
        /// <param name="authority">Optional. The specific authority to use to
        /// authenticate a user. For example, in Windows it would be a Windows 
        /// Domain</param>
        /// <returns>true when the username and password are valid</returns>
        public bool LogonUser(string userName, string password, string authority)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Required by IAuthenticationExtension. The report server calls the 
        /// GetUserInfo methodfor each request to retrieve the current user 
        /// identity.
        /// </summary>
        /// <param name="userIdentity">represents the identity of the current 
        /// user. The value of IIdentity may appear in a user interface and 
        /// should be human readable</param>
        /// <param name="userId">represents a pointer to a unique user identity
        /// </param>
        //original method that was added in the code.
        public void GetUserInfo(out IIdentity userIdentity, out IntPtr userId)
        {
            // initialize a pointer to the current user id to zero
            userId = IntPtr.Zero;

            // If the current user identity is not null,
            // set the userIdentity parameter to that of the current user 
            if (HttpContext.Current != null
                  && HttpContext.Current.User != null)
            {
                userIdentity = HttpContext.Current.User.Identity;
                int hashCode = userIdentity.Name.GetHashCode();
                userId = new IntPtr(hashCode);
            }
            else
            // The current user identity is null. This happens when the user attempts an anonymous logon.
            // Although it is ok to return userIdentity as a null reference, it is best to throw an appropriate
            // exception for debugging purposes.
            // To configure for anonymous logon, return a Gener
            {
                //System.Diagnostics.Debug.Assert(false, "Warning: userIdentity is null! Modify your code if you wish to support anonymous logon.");
                throw new NullReferenceException("Anonymous logon is not configured. userIdentity should not be null!");
            }

        }

        //adding new GetUserInfo method for IAuthenticationExtension2
        public void GetUserInfo(IRSRequestContext requestContext, out IIdentity userIdentity, out IntPtr userId)
        {
            // initialize a pointer to the current user id to zero
            userId = IntPtr.Zero;
            //HttpContext.Current.Response.RedirectLocation
            userIdentity = null;
            if (requestContext.User != null)
            {
                userIdentity = requestContext.User;
                int hashCode = userIdentity.Name.GetHashCode();
                userId = new IntPtr(hashCode);
            }

        }

        /// <summary>
        /// The IsValidPrincipalName method is called by the report server when 
        /// the report server sets security on an item. This method validates 
        /// that the user name is valid for Windows.The principal name needs to 
        /// be a user, group, or builtin account name.
        /// </summary>
        /// <param name="principalName">A user, group, or built-in account name
        /// </param>
        /// <returns>true when the principle name is valid</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public bool IsValidPrincipalName(string principalName)
        {
            return true;
        }
    }
}