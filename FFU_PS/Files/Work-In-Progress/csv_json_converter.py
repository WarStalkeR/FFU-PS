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
    csv_content = csv_content.replace('\\t', '\t').replace('\\n', '\n')
    new_filepath = os.path.splitext(filepath)[0] + '.csv'
    with open(new_filepath, 'w') as file:
        file.write(csv_content)
    print(f"Converted to CSV: {new_filepath}")

def process_file_tojson(filepath):
    with open(filepath, 'r') as file:
        content = file.read()
    # Convert '\t' to tabs and '\n' to new lines
    content = content.replace('\t', '\\t').replace('\n', '\\n')
    json_data = {
        "m_Name": os.path.splitext(os.path.basename(filepath))[0],
        "m_Script": ""
    }
    json_string = json.dumps(json_data, indent=2, ensure_ascii=False)
    json_string = json_string.replace(r'"m_Script": ""', f'"m_Script": "{content}"')
    new_filepath = os.path.splitext(filepath)[0] + '.json'
    with open(new_filepath, 'w') as file:
        file.write(json_string)
    print(f"Converted to JSON: {new_filepath}")

def main():
    parser = argparse.ArgumentParser(description="Automatically convert between CSV and JSON formats.")
    parser.add_argument('filepath', help="Path to the file to process")

    args = parser.parse_args()
    filepath = args.filepath

    if not os.path.isfile(filepath):
        print(f"File not found: {filepath}")
        return

    file_extension = os.path.splitext(filepath)[1].lower()

    if file_extension == '.csv':
        process_file_tojson(filepath)
    elif file_extension == '.json':
        process_file_tocsv(filepath)
    else:
        print("Unsupported file extension. Please provide a file with .csv or .json extension.")

if __name__ == "__main__":
    main()