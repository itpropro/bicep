
import type { PropsWithChildren } from "react";
import styled from "styled-components";

import type { Transient } from "./types";
import type { Dimension, Position } from "../core/types";
import useDrag from "../hooks/useDrag";

export type NodeProps = PropsWithChildren<{
  id: string;
  zIndex: number;
  position: Position;
  dimension: Dimension;
}>;

type $NodeProps = Transient<
  Pick<NodeProps, "zIndex" | "position" | "dimension">
>;

const $Node = styled.div.attrs<$NodeProps>(
  ({ $zIndex, $position, $dimension }) => ({
    style: {
      transform: `translate(${$position.x}px, ${$position.y}px)`,
      zIndex: $zIndex,
      width: `${$dimension.width}px`,
      height: `${$dimension.height}px`,
    },
  }),
)`
  cursor: default;
  display: flex;
  justify-content: center;
  align-items: center;
  transform-origin: 0 0;
  position: absolute;
  background: #1f1f1f1f;
  border-style: solid;
  border-color: black;
`;

export function Node({
  id,
  zIndex,
  position,
  dimension,
  children,
}: PropsWithChildren<NodeProps>) {
  const nodeRef = useDrag(id);

  return (
    <$Node
      ref={nodeRef}
      $zIndex={zIndex}
      $position={position}
      $dimension={dimension}
    >
      {children}
    </$Node>
  );
}
