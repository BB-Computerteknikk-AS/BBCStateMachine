using System;
using System.Drawing;
using DotNetGraph;
using DotNetGraph.Compiler;
using DotNetGraph.Edge;
using DotNetGraph.Node;
using DotNetGraph.SubGraph;

namespace no.bbc.StateMachine
{
    public class StateMachineGraphCompiler<STATE_T, INPUT_T>
        where STATE_T : struct, IConvertible
        where INPUT_T : struct, IConvertible
    {
        #region Public Constructor

        public StateMachineGraphCompiler(StateMachine<STATE_T, INPUT_T> stateMachine)
        {
            _stateMachine = stateMachine;
        }

        #endregion

        #region Private Fields

        private StateMachine<STATE_T, INPUT_T> _stateMachine;

        #endregion

        #region Public Methods

        public string CompileDotGraph(bool includeUnhandledTransitions = false)
        {
            var graph = new DotGraph("MyGraph", true);

            var handledTransitionsSubGraph = new DotSubGraph("cluster_0")
            {
                Style = DotSubGraphStyle.Dashed,
                Label = "Handled Transitions"
            };

            var transitionTable = _stateMachine.TransitionTable;

            foreach (Tuple<STATE_T, INPUT_T> key in transitionTable.Keys)
            {
                STATE_T state = key.Item1;
                INPUT_T input = key.Item2;
                STATE_T output = transitionTable[key];

                bool isCurrentState = state.Equals(_stateMachine.CurrentState);

                var stateNode = new DotNode(state.ToString())
                {
                    Shape = DotNodeShape.Circle,
                    Label = state.ToString(),
                    Style = DotNodeStyle.Filled,                    
                };
               
                if(isCurrentState)
                {
                    stateNode.FontColor = new DotNetGraph.Attributes.DotFontColorAttribute().Color = Color.White;
                    stateNode.FillColor = new DotNetGraph.Attributes.DotFillColorAttribute().Color = Color.DarkGreen;
                    stateNode.Height = new DotNetGraph.Attributes.DotNodeHeightAttribute(2f);
                }

                var outputNode = new DotNode(output.ToString())
                {
                    Shape = DotNodeShape.Circle,
                    Label = output.ToString(),
                    Style = DotNodeStyle.Filled,
                };

                var inputNode = new DotEdge(stateNode, outputNode)
                {
                    Label = input.ToString(),
                    Style = isCurrentState ? new DotNetGraph.Attributes.DotEdgeStyleAttribute(DotEdgeStyle.Bold) : new DotNetGraph.Attributes.DotEdgeStyleAttribute(DotEdgeStyle.Solid)
                };

                handledTransitionsSubGraph.Elements.Add(stateNode);
                handledTransitionsSubGraph.Elements.Add(outputNode);
                handledTransitionsSubGraph.Elements.Add(inputNode);
            }

            graph.Elements.Add(handledTransitionsSubGraph);

            if (includeUnhandledTransitions)
            {
                var unhandledTransitionsSubGraph = new DotSubGraph("cluster_1")
                { 
                    Style = DotSubGraphStyle.Dashed,
                    Label = "Unhandled Transitions"
                };

                var unhandledTransitions = _stateMachine.GetUnhandledTransitions();

                foreach (StateMachineTransition<STATE_T, INPUT_T> key in unhandledTransitions)
                {
                    STATE_T state = key.State;
                    INPUT_T input = key.Input;
                    STATE_T output = key.Output; 

                    var stateNode = new DotNode("unhandled_" +state.ToString())
                    {
                        Shape = DotNodeShape.Circle,
                        Label = state.ToString(),
                        Style = DotNodeStyle.Filled, 
                    };

                    var outputNode = new DotNode("unhandled_" + output.ToString())
                    {
                        Shape = DotNodeShape.Circle,
                        Label = output.ToString(),
                        Style = DotNodeStyle.Filled, 
                    };

                    var inputNode = new DotEdge(stateNode, outputNode)
                    {
                        Label = input.ToString(), 
                    };

                    unhandledTransitionsSubGraph.Elements.Add(stateNode);
                    unhandledTransitionsSubGraph.Elements.Add(outputNode);
                    unhandledTransitionsSubGraph.Elements.Add(inputNode);
                }

                graph.Elements.Add(unhandledTransitionsSubGraph);
            }

            var compiler = new DotCompiler(graph);

            var dot = compiler.Compile(indented: true);

            return dot;
        } 

        #endregion
    }
}
