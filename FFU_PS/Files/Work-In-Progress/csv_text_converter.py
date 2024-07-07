import argparse
import os

def process_file_tocsv(filepath):
    with open(filepath, 'r') as file:
        content = file.read()
    # Convert '\t' to tabs and '\n' to new lines
    content = content.replace('\\t', '\t').replace('\\n', '\n') #.replace('\\r', '\r')
    new_filepath = os.path.splitext(filepath)[0] + '.csv'
    with open(new_filepath, 'w') as file:
        file.write(content)
    print(f"Converted to CSV: {new_filepath}")

def process_file_totxt(filepath):
    with open(filepath, 'r') as file:
        content = file.read()
    # Convert tabs and new lines to '\t' and '\n'
    content = content.replace('\t', '\\t').replace('\n', '\\n') #.replace('\r', '\\r')
    new_filepath = os.path.splitext(filepath)[0] + '.txt'
    with open(new_filepath, 'w') as file:
        file.write(content)
    print(f"Converted to TXT: {new_filepath}")

def main():
    parser = argparse.ArgumentParser(description="Automatically convert between CSV and TXT formats.")
    parser.add_argument('filepath', help="Path to the file to process")

    args = parser.parse_args()
    filepath = args.filepath

    if not os.path.isfile(filepath):
        print(f"File not found: {filepath}")
        return

    file_extension = os.path.splitext(filepath)[1].lower()

    if file_extension == '.csv':
        process_file_totxt(filepath)
    elif file_extension == '.txt':
        process_file_tocsv(filepath)
    else:
        print("Unsupported file extension. Please provide a file with .csv or .txt extension.")

if __name__ == "__main__":
    main()
