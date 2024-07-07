import argparse
import os
import json
import re

def process_file_tocsv(filepath):
    with open(filepath, 'r') as file:
        content = file.read()
    # Extract the m_Script raw value manually
    pattern = r'"m_Script"\s*:\s*"((?:\\.|[^"\\])*)"'
    match = re.search(pattern, content)
    if match:
        m_script_raw = match.group(1)
    else:
        print("m_Script not found in the JSON file.")
        return
    json_data = json.loads(content)
    csv_content = m_script_raw
    # Convert '\t' to tabs and '\n' to new lines
    csv_content = csv_content.replace('\\t', '\t').replace('\\n', '\n').replace('\\r', '')
    new_filepath = os.path.splitext(filepath)[0] + '.txt'
    with open(new_filepath, 'w') as file:
        file.write(csv_content)
    print(f"Converted to TXT: {new_filepath}")

def main():
    parser = argparse.ArgumentParser(description="Automatically convert JSON to CSV format.")
    parser.add_argument('filepath', help="Path to the file to process")

    args = parser.parse_args()
    filepath = args.filepath

    if not os.path.isfile(filepath):
        print(f"File not found: {filepath}")
        return

    file_extension = os.path.splitext(filepath)[1].lower()

    if file_extension == '.json':
        process_file_tocsv(filepath)
    else:
        print("Unsupported file extension. Please provide a file with .json extension.")

if __name__ == "__main__":
    main()