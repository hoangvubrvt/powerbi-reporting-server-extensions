#region
// Copyright (c) 2016 Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License (MIT)
/*============================================================================
  File:     Authorization.cs

  Summary:  Demonstrates an implementation of an authorization 
            extension.
------------------------------------------------------------------------------
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

using System.IO;
using Microsoft.ReportingServices.Interfaces;
using System.Runtime.Serialization.Formatters.Binary;

namespace PowerBI.ReportingServer.Extensions.OIDC
{
   public sealed partial class Authorization: IAuthorizationExtension
    {
        public string LocalizedName => null; // Return a localized name for this extension

        /// <summary>
        /// Returns a security descriptor that is stored with an individual 
        /// item in the report server database. 
        /// </summary>
        /// <param name="acl">The access code list (ACL) created by the report 
        /// server for the item. It contains a collection of access code entry 
        /// (ACE) structures.</param>
        /// <param name="itemType">The type of item for which the security 
        /// descriptor is created.</param>
        /// <param name="stringSecDesc">Optional. A user-friendly description 
        /// of the security descriptor, used for debugging. This is not stored
        /// by the report server.</param>
        /// <returns>Should be implemented to return a serialized access code 
        /// list for the item.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public byte[] CreateSecurityDescriptor(
          AceCollection acl,
          SecurityItemType itemType,
          out string stringSecDesc)
        {
            // Creates a memory stream and serializes the ACL for storage.
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream result = new MemoryStream())
            {
                bf.Serialize(result, acl);
                stringSecDesc = null;
                return result.GetBuffer();
            }
        }

        // Used to deserialize the ACL that is stored by the report server.
        private AceCollection DeserializeAcl(byte[] secDesc)
        {
            AceCollection acl = new AceCollection();
            if (secDesc != null)
            {
                BinaryFormatter bf = new BinaryFormatter();
                using (MemoryStream sdStream = new MemoryStream(secDesc))
                {
                    acl = (AceCollection)bf.Deserialize(sdStream);
                }
            }

            return acl;
        }
    }
}
