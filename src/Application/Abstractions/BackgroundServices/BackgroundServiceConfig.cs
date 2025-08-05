using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Abstractions.BackgroundServices;
public class BackgroundServiceConfig
{
    public bool Enabled { get; set; } = true;
    public int IntervalSeconds { get; set; } = 60;
}
