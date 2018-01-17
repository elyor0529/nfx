
/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2018 Agnicore Inc. portions ITAdapter Corp. Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
</FILE_LICENSE>*/


/* NFX by ITAdapter
 * Originated: 2006.01
 * Revision: NFX 1.0  4/12/2014 8:56:57 PM
 * Author: Denis Latushkin<dxwizard@gmail.com>
 */
using System;

using NFX;
using NFX.Security;

namespace NFX.Web.Pay.Stripe
{
  public class StripeSession: PaySession
  {
    #region .ctor
      public StripeSession(PaySystem paySystem, StripeConnectionParameters cParams, IPaySessionContext context = null)
        : base(paySystem, cParams, context) { }
    #endregion

    #region Properties
      public string Email
      {
        get
        {
          if (!IsValid) return string.Empty;
          var cred = User.Credentials as StripeCredentials;
          if (cred == null) return string.Empty;
          return cred.Email;
        }
      }

      public string SecretKey
      {
        get
        {
          if (!IsValid) return string.Empty;
          var cred = User.Credentials as StripeCredentials;
          if (cred == null) return string.Empty;
          return cred.SecretKey;
        }
      }
    #endregion
  }
}
