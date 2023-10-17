import { Accordion as AccordionComponent } from "./Accordion";
import { AccordionItem } from "./AccordionItem";
import { AccordionItemHeader } from "./AccordionItemHeader";
import { AccordionItemContent } from "./AccordionItemContent";
import { useAccordionItem } from "./use-accordion-item";

const Accordion = Object.assign(AccordionComponent, {
  Item: AccordionItem,
  Header: AccordionItemHeader,
  Content: AccordionItemContent,
});

export { Accordion, useAccordionItem };
