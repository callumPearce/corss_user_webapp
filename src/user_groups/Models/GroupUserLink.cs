using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace user_groups.Models
{
    public class GroupUserLink
    {
        public int GroupUserLinkID { get; set; }
        public string ApplicationUserID { get; set; }
        public bool Active { get; set; }

        public int GroupItemID { get; set; }

        public GroupItem GroupItem { get; set; }
    }
}
