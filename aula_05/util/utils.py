import os

def renomear_para_minusculos(caminho):

    for arquivo in os.listdir(caminho):
        origem = os.path.join(caminho,arquivo)
        destino = os.path.join(caminho,arquivo.lower())
        if origem != destino:
            os.rename(origem,destino)
        if os.path.isdir(destino):
            renomear_para_minusculos(destino)


def renomear_para_maiusculos(caminho):

    for arquivo in os.listdir(caminho):
        origem = os.path.join(caminho,arquivo)
        destino = os.path.join(caminho,arquivo.upper())
        if origem != destino:
            os.rename(origem,destino)
        if os.path.isdir(destino):
            renomear_para_minusculos(destino)