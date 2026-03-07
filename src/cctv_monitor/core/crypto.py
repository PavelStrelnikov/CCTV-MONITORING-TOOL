from cryptography.fernet import Fernet


def encrypt_value(plain_text: str, key: str) -> str:
    f = Fernet(key.encode())
    return f.encrypt(plain_text.encode()).decode()


def decrypt_value(encrypted_text: str, key: str) -> str:
    f = Fernet(key.encode())
    return f.decrypt(encrypted_text.encode()).decode()
