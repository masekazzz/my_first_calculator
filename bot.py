import config
import telebot
import json
import re
from telebot import types

keyboard = types.ReplyKeyboardMarkup()
last_number_button = types.KeyboardButton("Последнее значение")
keyboard.add(last_number_button)

bot = telebot.TeleBot(config.token)

f = open('users.json', 'r')
users = json.loads(f.read())
f.close()


def write_to_json():
    global users, f
    json_data = json.dumps(users, indent=6)
    f = open("users.json", 'w')
    f.write(json_data)
    f.close()


@bot.message_handler(func=lambda message: message.text == "Последнее значение")
def send_last(message):
    bot.send_message(message.chat.id, str(users[str(message.chat.id)]))


@bot.message_handler(content_types=["text"])
def calculate_by_exec(message):
    if not re.fullmatch(r"[0-9()\-+*%/]*", message.text):
        bot.send_message(message.chat.id, "Это не является допустимым для калькулятора выражением")
        return
    if str(message.chat.id) not in users.keys():
        users.update({str(message.chat.id): 0})
    # Чтобы exec корректно работал с локальными переменными.
    local_namespace = {}
    try:
        exec("res = " + message.text, local_namespace)
    except:
        bot.send_message(message.chat.id, "Это не является допустимым для калькулятора выражением")
        return
    bot.send_message(message.chat.id, local_namespace['res'], reply_markup=keyboard)
    users[str(message.chat.id)] = local_namespace['res']
    write_to_json()


if __name__ == '__main__':
    bot.infinity_polling()
