﻿using System;

namespace SetCover.Objects
{
    /// <summary>
    /// Main object for generating the bipartite graph
    /// </summary>
    public class Node : IComparable
    {
        private static int IDNum;

        public enum NodeTypeName
        {
            Peptide = 0,
            Protein = 1,
            PeptideGroup = 2,
            ProteinGroup = 3,
            Cluster = 4,
            Other = 5
        }

        public NodeTypeName NodeType { get; set; }
        public int Id { get; set; }
        public NodeChildren<Node> Children { get; set; }
        public string NodeName { get; set; }

        // ReSharper disable once UnusedMember.Global

        /// <summary>
        /// Constructor
        /// </summary>
        public Node() : this(NodeTypeName.Other)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Node(NodeTypeName nodeType)
        {
            NodeType = nodeType;
            if (NodeType == NodeTypeName.Other)
                Console.WriteLine("Found other");

            Id = System.Threading.Interlocked.Increment(ref IDNum);
            NodeName = string.Empty;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Node(NodeTypeName nodeType, string nodeName) : this(nodeType)
        {
            NodeName = nodeName;
            Children = new NodeChildren<Node>();
        }

        // Use number of children to sort.
        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            if (obj is Node otherNode)
            {
                var childComparison = Children.Count.CompareTo(otherNode.Children.Count);
                if (childComparison != 0)
                    return childComparison;

                if (this is ProteinGroup thisProteinGroup && obj is ProteinGroup otherProteinGroup)
                {
                    var untakenPepComparison = thisProteinGroup.UntakenPeptides.CompareTo(otherProteinGroup.UntakenPeptides);
                    if (untakenPepComparison != 0)
                        return untakenPepComparison;
                }

                return Id.CompareTo(otherNode.Id);
            }

            throw new ArgumentException("Object is not a Node!");
        }

        public override string ToString()
        {
            // Protein 1: ProteinName, 8 children
            // Protein 2: ProteinName, 1 child
            // Peptide 5: ProteinName, 2 children
            var childLabel = Children.Count == 1 ? "child" : "children";
            return string.Format("{0} {1}: {2}, {3} {4}", NodeType, Id, NodeName, Children.Count, childLabel);
        }
    }
}
