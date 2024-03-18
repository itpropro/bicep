import type { ConfigState, ImmerStateCreator } from "./types";

export const createGraphSlice: ImmerStateCreator<ConfigState> = (set) => ({
  edgeType: "Straight",
  nodeType: "Compact",

  setEdgeType: (edgeType) =>
    set(({ config }) => {
      config.edgeType = edgeType;
    }),
  setNodeType: (nodeType) =>
    set(({ config }) => {
      config.nodeType = nodeType;
    }),
});
