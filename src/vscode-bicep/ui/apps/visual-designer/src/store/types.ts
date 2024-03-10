import "zustand/middleware/immer";
import type { Box, Position } from "../math/geometry";
import type { StateCreator } from "zustand";

export interface NodeState {
  id: string;
  parentId?: string;
  childIds: string[];
  edgeIds: string[];
  boundingBox: Box;
}

export interface EdgeState {
  id: string;
  sourceId: string;
  targetId: string;
  sourceIntersection?: Position;
  targetIntersection?: Position;
}
export interface GraphState {
  position: Position;
  scale: number;
  nodesById: Record<string, NodeState>;
  edgesById: Record<string, EdgeState>;

  translateTo: (position: Position) => void;
  scaleTo: (scale: number) => void;

  moveNode: (nodeId: string, dx: number, dy: number) => void;
  addNode: (nodeId: string, position: Position) => void;
}

export interface ConfigState {
  edgeType: "Straight" | "Bezier" | "Elbow";
  nodeType: "Compact" | "Informative";
}

export interface AppState {
  theme: "Default" | "Dark" | "Light" | "HighContrast";
  graph: GraphState;
  graphConfig: ConfigState;
}

export type ImmerStateCreator<T> = StateCreator<AppState, [["zustand/immer", never], never], [], T>;
