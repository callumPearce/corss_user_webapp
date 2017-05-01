using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace user_groups.Models
{
    public class GroupItem
    {
        public int GroupItemID { get; set; }
        public string GroupName { get; set; }
        public int MemberCount { get; set; }
        public string Host { get; set; }

        public ICollection<GroupUserLink> GroupUserLinks { get; set; }
    }
}
