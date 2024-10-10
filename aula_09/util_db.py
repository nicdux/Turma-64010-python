import pandas as pd
import sqlite3


def salva_no_bd(df,nome_tabela,nome_db):
    ''''Essa função é utilizada pra criar uma nova tabela
      no banco de dados escolhido'''
    
    conn = sqlite3.connect(nome_db)
    df.to_sql(nome_tabela,conn,if_exists='replace',index=False)
    conn.close()
    return True

def carrega_do_db(nome_tabela,nome_db):
    conn = sqlite3.connect(nome_db)
    query = f'SELECT * FROM {nome_tabela}'
    df = pd.read_sql(query,conn)
    conn.close()

    return df

def mostra_tabelas_bd(nome_db):
    conn = sqlite3.connect(nome_db)
    query = "SELECT name FROM sqlite_master WHERE type='table'"
    schema = pd.read_sql_query(query, conn)

    conn.close()
    return schema