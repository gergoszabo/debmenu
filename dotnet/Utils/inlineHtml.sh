#!/usr/bin/env bash
set -euo pipefail

template_file="${1:-template.html}"
namespace="${2:-debmenu}"
class_name="${3:-Html}"
property_name="${4:-Template}"

if [[ ! -f "$template_file" ]]; then
  echo "Template file not found: $template_file" >&2
  exit 1
fi

content=$(sed 's/"/""/g' "$template_file")

cat <<EOF > "${class_name}.cs"
namespace $namespace;

public static class $class_name
{
    public static string $property_name => @"
$content
";
}
EOF
