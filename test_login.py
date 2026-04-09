import requests
import json

url = "http://localhost:5189/api/Auth/login"
payload = {
    "username": "admin",
    "password": "admin123"
}
headers = {
    "Content-Type": "application/json"
}

try:
    response = requests.post(url, data=json.dumps(payload), headers=headers)
    print(f"Status Code: {response.status_code}")
    print(f"Response: {response.text}")
except Exception as e:
    print(f"Error: {e}")
