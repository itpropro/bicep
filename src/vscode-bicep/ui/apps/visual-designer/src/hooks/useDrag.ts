import { D3DragEvent, SubjectPosition, drag } from "d3-drag";
import { select } from "d3-selection";
import { useEffect, useRef } from "react";

import { graphStore } from "../store/graph-slice";

export default function useDrag(nodeId: string) {
  const elementRef = useRef<HTMLDivElement>(null);
  const moveNode = graphStore.use.graph().moveNode;

  useEffect(() => {
    if (elementRef.current) {
      const selection = select(elementRef.current as Element);
      const dragBehavior = drag().on(
        "drag",
        (event: D3DragEvent<Element, null, SubjectPosition>) => {
          moveNode(nodeId, event.dx, event.dy);
        },
      );

      selection.call(dragBehavior);

      return () => {
        selection.on("drag", null);
      };
    }
  }, [nodeId, moveNode]);

  return elementRef;
}
