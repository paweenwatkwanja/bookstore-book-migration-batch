#!/usr/bin/env bash
set -euo pipefail

mongosh <<'EOF'
use bookstore;
db.createCollection("books");
db.books.createIndex({ Isbn: 1 }, { unique: true, name: "uniq_isbn" });
EOF
