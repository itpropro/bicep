import {
  createBox,
  createPoint,
  createSegment,
  findFirstSegmentBoxIntersection,
  getMaxX,
  getMaxY,
  getMinX,
  getMinY,
  pointsEqual,
  translateBox,
} from "../math/geometry";
import { equal, equalsZero } from "../math/operators/comparison";

import type { GraphState, ImmerStateCreator, NodeState } from "./types";

function* enumerateAncestors(graph: GraphState, node: NodeState) {
  while (node.parentId) {
    yield graph.nodesById[node.parentId];
  }
}

function* enumerateChildren(graph: GraphState, node: NodeState) {
  for (const childId of node.childIds) {
    yield graph.nodesById[childId];
  }
}

function* enumerateDescendants(graph: GraphState, node: NodeState): Generator<NodeState> {
  for (const child of enumerateChildren(graph, node)) {
    yield child;
    yield* enumerateDescendants(graph, child);
  }
}

function updateLinkedEdges(graph: GraphState, node: NodeState) {
  for (const edgeId of node.edgeIds ?? []) {
    const edge = graph.edgesById[edgeId];
    const sourceNode = graph.nodesById[edge.sourceId];
    const targetNode = graph.nodesById[edge.targetId];
    const centerCenterSegment = createSegment(sourceNode.boundingBox.center, targetNode.boundingBox.center);

    edge.sourceIntersection = findFirstSegmentBoxIntersection(centerCenterSegment, sourceNode.boundingBox);
    edge.targetIntersection = findFirstSegmentBoxIntersection(centerCenterSegment, targetNode.boundingBox);
  }
}

export const createGraphSlice: ImmerStateCreator<GraphState> = (set) => ({
  position: { x: 0, y: 0 },
  scale: 1,
  nodesById: {},
  edgesById: {},

  translateTo: (position) =>
    set(({ graph }) => {
      graph.position = position;
    }),

  scaleTo: (scale) =>
    set(({ graph }) => {
      graph.scale = scale;
    }),

  moveNode: (nodeId, dx, dy) => {
    set(({ graph }) => {
      dx = dx / graph.scale;
      dy = dy / graph.scale;

      if (equalsZero(dx) && equalsZero(dy)) {
        return;
      }

      const node = graph.nodesById[nodeId];

      // 1. Update the bounding box and edges of the node and its descendants.
      node.boundingBox = translateBox(node.boundingBox, dx, dy);
      updateLinkedEdges(graph, node);

      for (const descendant of enumerateDescendants(graph, node)) {
        descendant.boundingBox = translateBox(descendant.boundingBox, dx, dy);
        updateLinkedEdges(graph, descendant);
      }

      // Update bounding boxes of ancestors.
      for (const ancestor of enumerateAncestors(graph, node)) {
        let x = Infinity;
        let y = Infinity;

        for (const child of enumerateChildren(graph, ancestor)) {
          x = Math.min(x, getMinX(child.boundingBox));
          y = Math.min(y, getMinY(child.boundingBox));
        }

        let width = 0;
        let height = 0;

        for (const child of enumerateChildren(graph, ancestor)) {
          width = Math.max(width, getMaxX(child.boundingBox) - x);
          height = Math.max(height, getMaxY(child.boundingBox) - y);
        }

        x -= 20;
        y -= 20;

        width += 20 * 2;
        height += 20 * 2;

        const center = createPoint(x + width / 2, y + height / 2);

        if (
          equal(ancestor.boundingBox.width, width) &&
          equal(ancestor.boundingBox.height, height) &&
          pointsEqual(ancestor.boundingBox.center, center)
        ) {
          return;
        }

        ancestor.boundingBox = createBox(center, width, height);
        updateLinkedEdges(graph, ancestor);
      }
    });
  },

  addNode: (nodeId, position) => {
    set(({ graph }) => {
      const x = (position.x - graph.position.x) / graph.scale;
      const y = (position.y - graph.position.y) / graph.scale;
      const boundingBox = createBox(createPoint(x, y), 100, 100);

      graph.nodesById[nodeId] = {
        id: nodeId,
        childIds: [],
        edgeIds: [],
        boundingBox,
      };
    });
  },
});
