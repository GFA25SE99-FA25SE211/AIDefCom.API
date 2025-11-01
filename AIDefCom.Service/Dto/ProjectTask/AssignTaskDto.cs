using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Dto.ProjectTask
{
    public class AssignTaskDto
    {
        public string AssignedToId { get; set; } = string.Empty;
        public string AssignedById { get; set; } = string.Empty;
    }
}
