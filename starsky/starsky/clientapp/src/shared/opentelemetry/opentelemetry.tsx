import { registerInstrumentations } from "@opentelemetry/instrumentation";
import { SimpleSpanProcessor, WebTracerProvider } from "@opentelemetry/sdk-trace-web";
import { SemanticResourceAttributes } from "@opentelemetry/semantic-conventions";
import { Resource } from "@opentelemetry/resources";
import { OTLPTraceExporter } from "@opentelemetry/exporter-trace-otlp-http";
import { FetchInstrumentation } from "@opentelemetry/instrumentation-fetch";
import * as React from "react";
import { GetCookie } from "../cookie/get-cookie.ts";

export default function TraceProvider({ children }: Readonly<{ children: React.ReactNode }>) {
  const collectorExporter = new OTLPTraceExporter({
    headers: {
      "X-XSRF-TOKEN": GetCookie("X-XSRF-TOKEN")
    },
    url: "/starsky/api/open-telemetry/trace"
  });

  const provider = new WebTracerProvider({
    resource: new Resource({
      [SemanticResourceAttributes.SERVICE_NAME]: "starsky-client-app"
    })
  });

  const fetchInstrumentation = new FetchInstrumentation({});

  fetchInstrumentation.setTracerProvider(provider);

  provider.addSpanProcessor(new SimpleSpanProcessor(collectorExporter));

  provider.register();

  registerInstrumentations({
    instrumentations: [fetchInstrumentation],
    tracerProvider: provider
  });

  return <>{children}</>;
}
