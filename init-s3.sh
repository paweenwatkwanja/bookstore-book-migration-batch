#!/usr/bin/env bash
set -euo pipefail

ENDPOINT_URL="${AWS_ENDPOINT_URL:-http://localhost:4566}"
BUCKET_NAME="${S3_BUCKET_NAME:-bookstore-books}"
OBJECT_KEY="${S3_OBJECT_KEY:-books/BX-Books.csv}"
SAMPLE_CSV_PATH="${SAMPLE_CSV_PATH:-/seed-data/BX-Books-Sample.csv}"
MAX_ATTEMPTS=30

echo "Waiting for Floci at ${ENDPOINT_URL} to accept requests..."

attempt=1
until aws --endpoint-url="${ENDPOINT_URL}" s3 mb "s3://${BUCKET_NAME}" 2>/dev/null || \
      aws --endpoint-url="${ENDPOINT_URL}" s3api head-bucket --bucket "${BUCKET_NAME}" 2>/dev/null; do
  if [ "${attempt}" -ge "${MAX_ATTEMPTS}" ]; then
    echo "Floci did not become ready after ${MAX_ATTEMPTS} attempts." >&2
    exit 1
  fi
  attempt=$((attempt + 1))
  sleep 2
done

echo "Bucket s3://${BUCKET_NAME} is ready. Uploading sample CSV..."
aws --endpoint-url="${ENDPOINT_URL}" s3 cp "${SAMPLE_CSV_PATH}" "s3://${BUCKET_NAME}/${OBJECT_KEY}"

echo "init-s3.sh complete."
