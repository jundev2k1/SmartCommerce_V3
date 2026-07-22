#!/bin/sh
# Idempotent ILM policy setup for service log indices. Each service's Serilog
# Elasticsearch sink auto-registers its own index template (TemplateName =
# "{service}-logs-template", see BuildingBlock.Observability/Logging/SerilogBootstrap.cs)
# with `index.lifecycle.name: logs-ilm-policy` already set in TemplateCustomSettings, so
# this script only needs to create the policy itself - no index template here, to avoid
# colliding with Elasticsearch's own built-in system templates on broad wildcard patterns.
#
# Runs once via the `es-init` one-shot container after Elasticsearch is healthy.
#
# No rollover action: services write directly to date-suffixed indices
# ({service}-logs-yyyy.MM.dd), not a rollover alias/data stream, so `min_age` for the
# delete phase is measured from each index's own creation date.
set -eu

ES_URL="${ELASTICSEARCH_URL:-http://elasticsearch:9200}"

curl -sf -X PUT "$ES_URL/_ilm/policy/logs-ilm-policy" \
  -H 'Content-Type: application/json' \
  -d '{
    "policy": {
      "phases": {
        "delete": {
          "min_age": "14d",
          "actions": { "delete": {} }
        }
      }
    }
  }'

echo "Elasticsearch logs-ilm-policy applied."
