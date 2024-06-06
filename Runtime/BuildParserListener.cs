using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
namespace Kurisu.AkiBT.DSL
{
    public sealed class BuildParserListener : IParserListener, IDisposable
    {
        private static readonly ObjectPool<BuildParserListener> pool = new(() => new());
        private BuildVisitor visitor;
        private readonly List<SharedVariable> variables = new();
        private readonly List<NodeBehavior> nodes = new();
        public BuildParserListener Verbose(bool verbose)
        {
            visitor.Verbose = verbose;
            return this;
        }
        public void PushTopLevelExpression(NodeExprAST data)
        {
            visitor.VisitNodeExprAST(data);
            nodes.Add(visitor.nodeStack.Pop());
        }
        public void PushVariableDefinition(VariableDefineExprAST data)
        {
            visitor.VisitVariableDefineAST(data);
            variables.Add(visitor.variableStack.Pop());
        }
        /// <summary>
        /// Build behavior tree
        /// </summary>
        /// <param name="bindObject"></param>
        /// <param name="behaviorTree"></param>
        /// <returns></returns>
        public BehaviorTree Build(GameObject bindObject)
        {
            if (!bindObject.TryGetComponent<BehaviorTree>(out var behaviorTree))
                behaviorTree = bindObject.AddComponent<BehaviorTree>();
            behaviorTree.SharedVariables.AddRange(variables);
            var sequence = new Sequence();
            foreach (var node in nodes)
                sequence.AddChild(node);
            behaviorTree.Root = new Root() { Child = sequence };
            variables.Clear();
            nodes.Clear();
            return behaviorTree;
        }
        /// <summary>
        /// Build behavior tree so
        /// </summary>
        /// <returns></returns>
        public BehaviorTreeSO Build()
        {
            var instance = ScriptableObject.CreateInstance<BehaviorTreeSO>();
            instance.SharedVariables.AddRange(variables);
            var sequence = new Sequence();
            foreach (var node in nodes)
                sequence.AddChild(node);
            instance.root = new Root() { Child = sequence };
            variables.Clear();
            nodes.Clear();
            return instance;
        }
        /// <summary>
        /// Get pooled build listener
        /// </summary>
        /// <param name="buildVisitor">The build visitor attached to</param>
        /// <returns></returns>
        public static BuildParserListener GetPooled(BuildVisitor buildVisitor)
        {
            var listener = pool.Get();
            listener.visitor = buildVisitor;
            return listener;
        }
        public void Dispose()
        {
            visitor?.Dispose();
            visitor = null;
            variables.Clear();
            nodes.Clear();
            pool.Release(this);
        }
    }
}