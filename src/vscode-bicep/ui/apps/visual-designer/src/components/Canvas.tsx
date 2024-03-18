import { DragEvent } from "react";
import { styled } from "styled-components";

import { Graph } from "./Graph";
import usePanZoom from "../hooks/usePanZoom";
import { graphStore } from "../stores/graph-slice";

const $Canvas = styled.div`
  position: absolute;
  left: 0px;
  top: 0px;
  right: 0px;
  bottom: 0px;
  overflow: hidden;
`;

let nodeId = 0;

export default function Canvas() {
  const canvasRef = usePanZoom();
  const addNode = graphStore.use.graph().addNode;

  function handleDragOver(event: DragEvent<HTMLDivElement>) {
    event.preventDefault();
  }

  function handleDrop(event: DragEvent<HTMLDivElement>) {
    const fullyQualifiedResourceType = event.dataTransfer.getData("text");

    addNode(`${fullyQualifiedResourceType}/${nodeId++}`, {
      x: event.clientX,
      y: event.clientY,
    });
  }

  return (
    <$Canvas ref={canvasRef} onDragOver={handleDragOver} onDrop={handleDrop}>
      <Graph />
    </$Canvas>
  );
}
