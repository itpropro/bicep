import styled from "styled-components";

import { Edge } from "./Edge";
import { Node } from "./Node";
import { graphStore } from "../stores/graph-slice";

const $Graph = styled.div.attrs<{
  $x: number;
  $y: number;
  $scale: number;
}>(({ $x, $y, $scale }) => ({
  style: {
    transform: `translate(${$x}px,${$y}px) scale(${$scale})`,
  },
}))`
  transform-origin: 0 0;
  height: 0;
  width: 0;
`;

const $Svg = styled.svg`
  overflow: visible;
`;

export function Graph() {
  const { x, y } = graphStore.use.graph().position;
  const scale = graphStore.use.graph().scale;
  const nodes = graphStore.use.graph().nodes;
  const edges = graphStore.use.graph().edges;

  return (
    <$Graph $x={x} $y={y} $scale={scale}>
      {Object.values(nodes).map((node) => (
        <Node key={node.id} {...node}>
          {node.id}
        </Node>
      ))}
      <$Svg>
        <g>
          {Object.values(edges).map((edge) => (
            <Edge key={edge.id} {...edge} />
          ))}
        </g>
      </$Svg>
    </$Graph>
  );
}
