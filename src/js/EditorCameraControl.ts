import { Camera, Euler, Vector3 } from 'three'
import { ThreeApp } from './ThreeApp'

export class EditorCameraControl {
  public get camera(): Camera {
    return this.camera_
  }

  public get threeApp(): ThreeApp {
    return this.threeApp_
  }

  public rotationSpeed = 0.3
  public movementSpeed = 1

  private isMouseDown = false
  private camEuler = new Euler(0, 0, 0, 'YXZ')

  private controlButton = 2

  private keyForwards = 'KeyW'
  private keyBackwards = 'KeyS'
  private keyLeft = 'KeyA'
  private keyRight = 'KeyD'
  private keyUp = 'Space'
  private keyDown = 'ControlLeft'

  private forwardsDown = false
  private backwardsDown = false
  private leftDown = false
  private rightDown = false
  private upDown = false
  private downDown = false

  private movedWhileDown = false

  constructor(private threeApp_: ThreeApp, private camera_: Camera) {
    const elem = threeApp_.renderer.domElement

    elem.addEventListener('mousemove', this.onMouseMove)
    elem.addEventListener('mousedown', this.onMouseDown)
    elem.addEventListener('mouseup', this.onMouseUp)
    elem.addEventListener('contextmenu', this.onContextMenu)

    document.addEventListener('keydown', this.onKeyDown)
    document.addEventListener('keyup', this.onKeyUp)

    // TODO: keep track of mouseup even if the mouse is outside the browser window??
    window.addEventListener('blur', this.onBlur)

    threeApp_.addEventListener('animate', this.onAnimate)

    // TODO: init camEuler
    this.camEuler.y = this.camera_.rotation.y
    this.camEuler.x = this.camera_.rotation.x
  }

  private onAnimate = (): void => {
    // update cam position etc
    const forward = new Vector3(0, 0, -1).applyQuaternion(this.camera.quaternion)
    const left = new Vector3(-1, 0, 0).applyQuaternion(this.camera.quaternion)
    const up = new Vector3(0, 1, 0) // up down motion is not depending on camera orientation

    const offset = new Vector3()
    if (this.forwardsDown) {
      offset.add(forward)
    }
    if (this.backwardsDown) {
      offset.sub(forward)
    }
    if (this.leftDown) {
      offset.add(left)
    }
    if (this.rightDown) {
      offset.sub(left)
    }
    if (this.upDown) {
      offset.add(up)
    }
    if (this.downDown) {
      offset.sub(up)
    }

    offset.multiplyScalar(this.movementSpeed)
    this.camera.position.add(offset)
  }

  // prevent context menu from showing up if we moved the mouse while rightclicking
  private onContextMenu = (ev: MouseEvent): void => {
    if (ev.button === this.controlButton && this.movedWhileDown) {
      ev.preventDefault()
    }
  }

  private onMouseMove = (ev: MouseEvent): void => {
    // camerarotation should actually be updated in animate, but i think its fine in this case?
    if (this.isMouseDown) {
      this.movedWhileDown = true
      this.camEuler.y -= ev.movementX * this.rotationSpeed * 0.01
      this.camEuler.x -= ev.movementY * this.rotationSpeed * 0.01
      this.camEuler.x = Math.max(-Math.PI / 2, Math.min(Math.PI / 2, this.camEuler.x))

      this.camera_.quaternion.setFromEuler(this.camEuler)
    }
  }

  private onMouseDown = (ev: MouseEvent): void => {
    if (ev.button === this.controlButton) {
      this.isMouseDown = true
      this.movedWhileDown = false
    }
  }

  private onMouseUp = (ev: MouseEvent): void => {
    if (ev.button === this.controlButton) {
      this.isMouseDown = false
    }
  }

  private onBlur = (): void => {
    this.isMouseDown = false

    this.forwardsDown = false
    this.backwardsDown = false
    this.leftDown = false
    this.rightDown = false
  }

  private setKeyDown(ev: KeyboardEvent, down: boolean): void {
    switch (ev.code) {
      case this.keyForwards:
        this.forwardsDown = down
        break
      case this.keyBackwards:
        this.backwardsDown = down
        break
      case this.keyLeft:
        this.leftDown = down
        break
      case this.keyRight:
        this.rightDown = down
        break
      case this.keyUp:
        this.upDown = down
        break
      case this.keyDown:
        this.downDown = down
        break
    }
  }

  private onKeyDown = (ev: KeyboardEvent): void => {
    this.setKeyDown(ev, true)
  }

  private onKeyUp = (ev: KeyboardEvent): void => {
    this.setKeyDown(ev, false)
  }
}