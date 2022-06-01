using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetCore_Model_View_Cortol_Created.Utilities
{
    public class EmailDomainAttribute : ValidationAttribute // our custom attribute to check domain
    {
        private readonly string ourDomain;

        // pass our parameters to the Attribute 
        public EmailDomainAttribute(string ourDomain) 
        {
            this.ourDomain = ourDomain;
        }
        public override bool IsValid(object value)
        {
            string[] strings = value.ToString().Split("@"); // to make it possible to take @domainname.com from shenoda@domainname.com
            string domain = strings[1];
            return domain.ToUpper() == ourDomain.ToUpper();
        }
    }
}
