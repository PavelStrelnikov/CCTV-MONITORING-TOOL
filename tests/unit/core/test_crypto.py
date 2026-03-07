from cryptography.fernet import Fernet
from cctv_monitor.core.crypto import encrypt_value, decrypt_value


def test_encrypt_decrypt_roundtrip():
    key = Fernet.generate_key().decode()
    original = "my_secret_password"
    encrypted = encrypt_value(original, key)
    assert encrypted != original
    decrypted = decrypt_value(encrypted, key)
    assert decrypted == original


def test_encrypted_value_is_different_each_time():
    key = Fernet.generate_key().decode()
    enc1 = encrypt_value("password", key)
    enc2 = encrypt_value("password", key)
    assert enc1 != enc2  # Fernet uses random IV
