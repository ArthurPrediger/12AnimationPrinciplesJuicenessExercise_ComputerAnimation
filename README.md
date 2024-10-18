# 12AnimationPrinciplesJuicenessExercise_ComputerAnimation

Funcionalidades disponíveis:

Movimentação de câmera:

* WASD para translação da câmera para frente, esquerda, trás e direita respectivamente.

* Botão esquerdo do mouse pressionado + movimentação do mesmo para rotacionar a câmera.

Interações com objetos:

* Clique com o botão esquerdo do mouse nos objetos para eles saltarem. (Utiliza deformação de malha, funções de easing para a deformação e partículas)

* Clicar e segurar com o botão direito sobre um objeto e então arrastar e soltar o mesmo botão para o objeto se mover até a posição no mundo apontada pelo mouse no momento de soltura do botão. (Utiliza curvas de bézier para gerar o trajeto de movimentação dos objetos e função de easing enquanto o objeto é transladado)

* Colisão entre objetos geram partículas. (Utiliza um sistema de partículas com métodos para gerar e manter vivas as partículas baseadas em funções de easing)
