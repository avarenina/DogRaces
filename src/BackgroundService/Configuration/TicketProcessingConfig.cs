using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Abstractions.BackgroundServices;

namespace BackgroundService.Configuration;
public class TicketProcessingConfig : BackgroundServiceConfig
{
    public int BatchSize { get; set; }
}
