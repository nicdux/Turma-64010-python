 

class Retangulo:
    def __init__(self,base,altura):
        self.base = base
        self.altura = altura

    def area(self):
        return self.base *self.altura
    
    def perimetro(self):
        return 2*(self.base+self.altura)
    

class Circulo:
    def __init__(self,raio):
        self.raio = raio

    def area(self):
        return 3.14 *(self.raio**2)
    
    def perimetro(self):
        return 2 * (3.14*self.raio)