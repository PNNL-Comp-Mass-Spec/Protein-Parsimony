using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace SetCover
{
    class AlgBase 
    {
      //  protected ILog log = LogManager.GetLogger(typeof(AlgBase));
        public void ReadFile()
        {
            throw new NotImplementedException();
        }

        public virtual void RunAlgorithm(List<Node> inNode, List<Node> outNode)
        {
   //         log.Info("some message");
            
        }

    }
}
