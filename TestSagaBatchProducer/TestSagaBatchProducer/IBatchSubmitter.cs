using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestSagaBatchProducer;

public interface IBatchSubmitter
{
    Task<Guid> SubmitBatch(CancellationToken stoppingToken);
}