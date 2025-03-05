using System;
using System.Collections.Generic;

namespace UMonitorWebAPI.Models;

public partial class Role
{
    /// <summary>
    /// 0為administrator；1為engineer；2為user
    /// </summary>
    public int RoleId { get; set; }

    public string RoleName { get; set; } = null!;
}
