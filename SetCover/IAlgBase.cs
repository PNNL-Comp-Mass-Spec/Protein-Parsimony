using System;
namespace SetCover
{
    interface IAlgBase
    {
        void RunAlgorithm(System.Collections.Generic.List<Node> inNode, 
            System.Collections.Generic.List<Node> outNode);

        void RunAlgorithm(System.Collections.Generic.List<Node> pep,
            System.Collections.Generic.List<Node> protein);

        void RunAlgorithm(System.Collections.Generic.List<Node> inpeptide, 
            System.Collections.Generic.List<Node> inprotein, 
            ref System.Collections.Generic.List<Node> outpep,
            ref System.Collections.Generic.List<Node> outprot);
    }
}
